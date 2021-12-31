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
		let minDate = (await Database.execQuery(
				'SELECT MAX(date) as min FROM focus_stats', []
			)).rows[0].min;
		if(minDate === null) {
			minDate = new Date(0);
		} else {
			minDate.setMinutes(minDate.getMinutes() - 5); // Safety margin
		}

		const history 	= JSON.parse(fs.readFileSync(folder + 'history.json'));
		for(const name in history) {
			for(const exe in history[name]) {
				if(name === '' && exe === '') {
					continue;
				}

				for(const element of history[name][exe]) {
					const date = new Date(element.date);

					if(date.getTime() < minDate.getTime()) {
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
