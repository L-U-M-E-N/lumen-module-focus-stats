import fs from 'fs';
const folder = 'G:\\Lumen\\resources\\app\\data\\focus-stats\\';

const tags 		= JSON.parse(fs.readFileSync(folder + 'tags.json'));
const history 	= JSON.parse(fs.readFileSync(folder + 'history.json'));

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
		const dbEntries = (await Database.execQuery('SELECT date, duration FROM focus_stats')).rows;
		const dbEntriesMapped = {};
		for(const dbEntry of dbEntries) {
			dbEntriesMapped[dbEntry.date.getTime()] = dbEntry.duration;
		}

		const history 	= JSON.parse(fs.readFileSync(folder + 'history.json'));
		for(const name in history) {
			for(const exe in history[name]) {
				if(name === '' && exe === '') {
					continue;
				}

				for(const element of history[name][exe]) {
					const date = new Date(element.date);


					if(typeof dbEntriesMapped[date.getTime()] !== 'undefined') {
						if(dbEntriesMapped[date.getTime()] !== element.duration) {
							await Database.execQuery(
								'UPDATE focus_stats SET duration = $2 WHERE date = $1',
								[date, element.duration]
							);
						}

						continue;
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

		// Insert tags
		const tags 		= JSON.parse(fs.readFileSync(folder + 'tags.json'));
		for(const name in tags) {
			for(const exe in tags[name]) {
				for(const tag of tags[name][exe]) {
					if(data.filter(
						(elt) => elt.name === name && elt.exe === exe && elt.tag === tag
					).length > 0) {
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
}
