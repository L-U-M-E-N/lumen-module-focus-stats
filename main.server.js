import fs from 'fs';

const tags 		= JSON.parse(fs.readFileSync(config['dataFolder'] + 'tags.json'));
const history 	= JSON.parse(fs.readFileSync(config['dataFolder'] + 'history.json'));

function showData(name, dataset) {
	console.log(
		name.padEnd(20, ' ') + '| Data size: ' +
		Object.values(dataset).length + ' / '+
		Object.values(dataset).map((elt) => Object.values(elt)).flat(1).length + ' / '+
		Object.values(dataset).map((elt) => Object.values(elt)).flat(2).length
	);
}

export default class FocusStats {
	static init() {
		(async () => {
			await FocusStats.cleanDatabaseData();
			FocusStats.cleanFilesData();

			await FocusStats.update();

			clearInterval(FocusStats.interval);
			FocusStats.interval = setInterval(FocusStats.update, 60 * 60 * 1000); // Update every hour
		})();
	}

	static close() {
		clearInterval(FocusStats.interval);
	}

	static async cleanDatabaseData() {
		log('Cleaning database data ...')

		for(const oldVal in config['cleaner']['substitutions']) {
			const newVal = config['cleaner']['substitutions'][oldVal];

			log(`	Database - Substitutions "${oldVal}" => "${newVal}"`);

			// Rename in history
			await Database.execQuery(`
				UPDATE focus_stats
				SET name = REPLACE(name, '${oldVal}', '${newVal}'), exe = REPLACE(exe, '${oldVal}', '${newVal}')
				WHERE exe LIKE '%${oldVal}%'
				OR 	 name LIKE '%${oldVal}%'
			`);

			// Remove tagging, it'll be re-push if not duplicate with right values
			await Database.execQuery(`
				DELETE FROM focus_stats_tags
					WHERE exe LIKE '%${oldVal}%'
					OR 	 name LIKE '%${oldVal}%'
			`);
		}

		for(const value of config['cleaner']['keepEndOnly']) {
			log(`	Database - Keep end only "${value}"`);

			// Rename in history
			await Database.execQuery(`
				UPDATE focus_stats SET exe = '${value}'
				WHERE exe LIKE '%${value}'
			`);
			await Database.execQuery(`
				UPDATE focus_stats SET name = '${value}'
				WHERE name LIKE '%${value}'
			`);

			// Remove tagging, it'll be re-push if not duplicate with right values
			await Database.execQuery(`
				DELETE FROM focus_stats_tags
					WHERE exe LIKE '%${value}'
					OR 	 name LIKE '%${value}'
			`);
		}

		for(const value of config['cleaner']['keepStartOnly']) {
			log(`	Database - Keep start only "${value}"`);

			// Rename in history
			await Database.execQuery(`
				UPDATE focus_stats SET exe = '${value}'
				WHERE exe LIKE '${value}%'
			`);
			await Database.execQuery(`
				UPDATE focus_stats SET name = '${value}'
				WHERE name LIKE '${value}%'
			`);

			// Remove tagging, it'll be re-push if not duplicate with right values
			await Database.execQuery(`
				DELETE FROM focus_stats_tags
					WHERE exe LIKE '${value}%'
					OR 	 name LIKE '${value}%'
			`);
		}

		log('Cleaned database data !')
	}

	static cleanString(str) {
		for(const value of config['cleaner']['keepEndOnly']) {
			if(str.endsWith(value)) {
				return value;
			}
		}

		for(const value of config['cleaner']['keepStartOnly']) {
			if(str.startsWith(value)) {
				return value;
			}
		}

		for(const oldVal in config['cleaner']['substitutions']) {
			const newVal = config['cleaner']['substitutions'][oldVal];

			str = str.replace(oldVal, newVal);
		}

		return str;
	}

	static cleanFilesData() {
		log('Cleaning file data ...')
		showData('history', history);
		showData('tags', tags);

		const newHistory = {};

		let i = 0;
		for(const exe in history) {
			log(`	File (history) - ${i++} - ${exe}`);
			const formattedExe = FocusStats.cleanString(exe);
			if(!newHistory[formattedExe]) {
				newHistory[formattedExe] = {};
			}

			for(const name in history[exe]) {
				const formattedName = FocusStats.cleanString(name);
				if(!newHistory[formattedExe][formattedName]) {
					newHistory[formattedExe][formattedName] = [];
				}

				for(const item of history[exe][name]) {
					newHistory[formattedExe][formattedName].push(item);
				}
			}
		}

		i = 0;
		const newTags = {};
		for(const exe in tags) {
			log(`	File (tags) - ${i++} - ${exe}`);
			const formattedExe = FocusStats.cleanString(exe);

			for(const name in tags[exe]) {
				const formattedName = FocusStats.cleanString(name);

				newTags[formattedExe] = newTags[formattedExe] || {};
				newTags[formattedExe][formattedName] = Array.from(new Set([...(newTags[formattedExe][formattedName] || []), ...tags[exe][name] ]));
			}
		}

		fs.writeFileSync(config['dataFolder'] + 'history.json', JSON.stringify(newHistory, null, 4))
		fs.writeFileSync(config['dataFolder'] + 'tags.json', JSON.stringify(newTags, null, 4))

		showData('history', newHistory);
		showData('tags', newTags);
		log('Cleaned file data !')
	}

	static async pushHistory() {
		log('Saving current focus stats ...', 'info');

		const dbEntries = (await Database.execQuery('SELECT date, duration, exe, name FROM focus_stats')).rows;
		const dbEntriesMapped = {};
		for(const dbEntry of dbEntries) {
			dbEntriesMapped[dbEntry.date.getTime()] = dbEntry;
		}

		const history 	= JSON.parse(fs.readFileSync(config['dataFolder'] + 'history.json'));
		for(const name in history) {
			for(const exe in history[name]) {
				if(name === '' && exe === '') {
					continue;
				}

				for(const element of history[name][exe]) {
					const date = new Date(element.date);

					if(typeof dbEntriesMapped[date.getTime()] !== 'undefined') {
						if(
							dbEntriesMapped[date.getTime()].duration === element.duration &&
							dbEntriesMapped[date.getTime()].name === name &&
							dbEntriesMapped[date.getTime()].exe === exe) {

							continue;
						}

						await Database.execQuery(
							'DELETE FROM focus_stats WHERE date = $1',
							[date]
						);
					}

					const [query, values] = Database.buildInsertQuery('focus_stats', {
						name,
						exe,
						date,
						duration: element.duration,
					});

					await Database.execQuery(
						query,
						values
					);
				}
			}
		}

		log('Saved current focus stats', 'info');
	}

	static async pushTags() {
		log('Saving tags focus stats assignements', 'info');

		// Get already inserted tags
		const data = (await Database.execQuery('SELECT * FROM focus_stats_tags')).rows;

		// TODO: delete data that has been removed on UI
		// We won't do it now because there's now way to remove tags from UI atm

		const dbTagsMapped = {};
		for(let row of data) {
			if(!dbTagsMapped[row.name]) {
				dbTagsMapped[row.name] = {};
			}
			if(!dbTagsMapped[row.name][row.exe]) {
				dbTagsMapped[row.name][row.exe] = [];
			}

			dbTagsMapped[row.name][row.exe].push(row.tag);
		}

		// Insert tags
		const tags 		= JSON.parse(fs.readFileSync(config['dataFolder'] + 'tags.json'));
		for(const name in tags) {
			for(const exe in tags[name]) {
				for(const tag of tags[name][exe]) {
					if(dbTagsMapped[name] && dbTagsMapped[name][exe] && dbTagsMapped[name][exe].includes(tag)) {
						continue;
					}

					const [query, values] = Database.buildInsertQuery('focus_stats_tags', {
						name,
						exe,
						tag
					});

					await Database.execQuery(
						query,
						values
					);
				}
			}
		}

		log('Saved tags focus stats assignements', 'info');
	}

	static async update() {
		try {
			await FocusStats.pushHistory();
			await FocusStats.pushTags();
		} catch(e) {
			console.error(e);
		}
	}
}
