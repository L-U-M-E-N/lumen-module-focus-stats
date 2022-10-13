import fs from 'fs';

const tags 		= JSON.parse(fs.readFileSync(config['dataFolder'] + 'tags.json'));
const history 	= JSON.parse(fs.readFileSync(config['dataFolder'] + 'history.json'));

export default class FocusStats {
	static init() {
		FocusStats.update();

		clearInterval(FocusStats.interval);
		FocusStats.interval = setInterval(FocusStats.update, 60 * 60 * 1000); // Update every hour
	}

	static close() {
		clearInterval(FocusStats.interval);
	}

	static async update() {
		try {
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
		} catch(e) {
			console.error(e);
		}
	}
}
