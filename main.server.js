const fs = require('fs');
const folder = '';

const tags 		= JSON.parse(fs.readFileSync(folder + 'tags.json'));
const history 	= JSON.parse(fs.readFileSync(folder + 'history.json'));

module.exports = class FocusStats {
	static init() {
		FocusStats.update();

		clearInterval(FocusStats.interval);
		FocusStats.interval = setInterval(FocusStats.update, 60 * 60 * 1000); // Update every hour
	}

	static close() {
		clearInterval(FocusStats.interval);
	}

	static async update() {
		const minDate = (await Database.execQuery(
				'SELECT MAX(date) as min FROM focus_stats', []
			)).rows[0].min;
		minDate.setHours(minDate.getHours() - 6); // Safety margin

		const history 	= JSON.parse(fs.readFileSync(folder + 'history.json'));
		for(const name in history) {
			for(const exe in history[name]) {
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

		// Clean tags
		await Database.execQuery('DELETE FROM focus_stats_tags');

		// Insert tags
		const tags 		= JSON.parse(fs.readFileSync(folder + 'tags.json'));
		for(const name in tags) {
			for(const exe in tags[name]) {
				for(const tag of tags[name][exe]) {
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

		log('Saved current focus stats', 'info');
	}
};