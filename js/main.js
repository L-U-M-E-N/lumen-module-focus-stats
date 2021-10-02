class FocusStats {
	static async init() {
		FocusStats.searchFilter = '';

		await FocusStats.reload();
	}

	static async reload() {
		await FocusStats.computeData();

		// Generate DOM
		FocusStats.regenerateList();
		FocusStats.regenerateTagList();

		FocusStats.setupAddTagsEvent();
		FocusStats.setupFilterEvents();
	}

	static async computeData() {
		const history = await AppDataManager.loadObject('focus-stats', 'history');
		const tagsData = await AppDataManager.loadObject('focus-stats', 'tags');

		FocusStats.computedData = {};
		for(const name in history) {
			for(const exe in history[name]) {
				if(!FocusStats.computedData[[name, exe]]) {
					FocusStats.computedData[[name, exe]] = {
						name,
						exe,
						totalDuration: history[name][exe].reduce((acc, val) => acc + val.duration, 0),
						history: history[name][exe],
						tags: (tagsData[name] && tagsData[name][exe]) ? tagsData[name][exe] : [],
					};
				}
			}
		}
	}

	static setupAddTagsEvent() {
		document.getElementById('assignTags').addEventListener('click', async() => {
			const newTags = document.getElementById('newTags').value.split(',').map((elt) => elt.trim().toLowerCase()).filter((elt) => elt !== '');

			const tagsData = await AppDataManager.loadObject('focus-stats', 'tags');
			for(const checkedElement of FocusStats.checked) {
				const name = checkedElement.name;
				const exe = checkedElement.exe;

				if(!tagsData[name]) {
					tagsData[name] = {};
				}

				if(!Array.isArray(tagsData[name][exe])) {
					tagsData[name][exe] = [];
				}

				tagsData[name][exe] = Array.from(new Set([...tagsData[name][exe], ...newTags]));
			}
			await AppDataManager.saveObject('focus-stats', 'tags', tagsData);

			await FocusStats.reload();
		});

		for(const element of Array.from(document.querySelectorAll('#main-focus-stats-data input[type=checkbox]'))) {
			element.addEventListener('change', () => {
				const exe = element.getAttribute('exe');
				const name = element.getAttribute('name');

				if(element.checked) {
					FocusStats.checked.push({exe, name});
				} else {
					FocusStats.checked = FocusStats.checked.filter((elt) => elt.name !== name && elt.exe !== exe);
				}
			});
		}
	}

	static setupFilterEvents() {
		const input = document.getElementById('main-focus-stats-search-input');
		input.addEventListener('change', () => {
			FocusStats.searchFilter = input.value.toLowerCase();
			FocusStats.reload();
		});

		// Reset event
		const checkAll = document.getElementById('checkAll');
		checkAll.addEventListener('change', () => {
			const everyCheckBox = Array.from(document.querySelectorAll('#main-focus-stats-data input[type=checkbox]'));
			everyCheckBox.map((elt) => {
				elt.checked = checkAll.checked;
				elt.dispatchEvent(new Event('change'));
			});
		});
	}

	static regenerateTagList() {
		const tagListElement = document.getElementsByClassName('main-focus-stats-tagList')[0];
		while(tagListElement.firstChild) {
			tagListElement.firstChild.remove();
		}

		for(const tag of FocusStats.tagsList) {
			const element = document.createElement('kbd');
			element.innerText = tag;

			// TODO: filter

			tagListElement.appendChild(element);
		}
	}

	static regenerateList() {
		FocusStats.checked = [];

		const tBody = document.getElementById('main-focus-stats-data');
		while(tBody.firstChild) {
			tBody.firstChild.remove();
		}

		FocusStats.tagsList = new Set();

		let list = Object.values(FocusStats.computedData);
		list.sort((a, b) => b.totalDuration - a.totalDuration);

		if(FocusStats.searchFilter !== '') {
			list = list.filter((elt) => elt.name.toLowerCase().includes(FocusStats.searchFilter) || elt.exe.toLowerCase().includes(FocusStats.searchFilter));
		}

		for(let i = 0; i < Math.min(100, list.length); i++) {
			tBody.appendChild(FocusStats.generateListItem(list[i]));
		}
	}

	static generateListItem(data, i) {
		const line = document.createElement('tr');

		const checkboxTd = document.createElement('td');
		const checkbox = document.createElement('input');
		checkbox.setAttribute('type', 'checkbox');
		checkbox.setAttribute('exe', data.exe);
		checkbox.setAttribute('name', data.name);
		checkboxTd.appendChild(checkbox);
		line.appendChild(checkboxTd);

		const name = document.createElement('td');
		name.innerText = data.name;
		line.appendChild(name);

		const exe = document.createElement('td');
		exe.innerText = data.exe;
		line.appendChild(exe);

		const totalDuration = document.createElement('td');
		totalDuration.innerText = Math.floor(data.totalDuration / 3600).toString().padStart(2, '0') + ':' + (Math.floor(data.totalDuration / 60) % 60).toString().padStart(2, '0') + ':' + (data.totalDuration % 60).toString().padStart(2, '0');
		line.appendChild(totalDuration);

		const tags = document.createElement('td');
		for(const tag of data.tags || []) {
			const kbd = document.createElement('kbd');
			kbd.innerText = tag;

			// TODO: remove popup

			tags.appendChild(kbd);

			if(!FocusStats.tagsList.has(tag)) {
				FocusStats.tagsList.add(tag);
			}
		}
		line.appendChild(tags);

		return line;
	}
}
window.addEventListener('load', FocusStats.init);