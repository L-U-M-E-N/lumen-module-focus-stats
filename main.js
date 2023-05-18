import fs from 'fs';
import path from 'path';
import getActiveProgram from './getActiveProgram.cjs';

let historyCache = [];
if(await AppDataManager.exists('focus-stats', 'historyCache')) {
	historyCache = await AppDataManager.loadObject('focus-stats', 'historyCache');
}
if(!(await AppDataManager.exists('focus-stats', 'tags'))) {
	await AppDataManager.saveObject('focus-stats', 'tags', {});
}
if(!(await AppDataManager.exists('focus-stats', 'history'))) {
	await AppDataManager.saveObject('focus-stats', 'history', {});
}

const config = ConfigManager.get('focus-stats', 'cleaner');

function cleanString(str) {
	for(const value of config['keepEndOnly']) {
		if(str.endsWith(value)) {
			return value;
		}
	}

	for(const value of config['keepStartOnly']) {
		if(str.startsWith(value)) {
			return value;
		}
	}

	for(const oldVal in config['substitutions']) {
		const newVal = config['substitutions'][oldVal];

		str = str.replace(oldVal, newVal);
	}

	return str.substring(0, 1000);
}

function addNewActivityToHistoryCache() {
	const currProgramExe  = cleanString(getActiveProgram.getActiveProgramExe());
	const currProgramName = cleanString(getActiveProgram.getActiveProgramName());

	// Skip invalid links, TODO: support linux
	if(currProgramExe.length < 'A:\\a.exe'.length || currProgramExe[1] !== ':' || currProgramExe[2] !== '\\') {
		return;
	}

	//console.log(new Date(), 'New activity:', currProgramName, getActiveProgram.getActiveProgramExe());
	//console.log(new Date(), 'Exe:', getActiveProgram.getActiveProgramExe());

	const lastActivity = historyCache[historyCache.length - 1] || {};
	if(lastActivity.name === currProgramName && lastActivity.exe === currProgramExe) {
		historyCache[historyCache.length - 1].duration++;
	} else {
		historyCache.push({
			name: currProgramName,
			exe: currProgramExe,
			date: new Date(),
			duration: 0,
		});
	}
}

async function persistCache() {
	await AppDataManager.saveObject('focus-stats', 'historyCache', historyCache);
}

async function persistFullData() {
	const history = await AppDataManager.loadObject('focus-stats', 'history');

	while(historyCache.length > 0) {
		const item = historyCache.pop();

		if(!history[item.exe]) {
			history[item.exe] = {};
		}

		if(!history[item.exe][item.name]) {
			history[item.exe][item.name] = [];
		}

		if(history[item.exe][item.name].find(x => ((new Date(x.date)).getTime()) === ((new Date(item.date)).getTime()))) { // Duplicate
			continue;
		}

		console.log('push data to history');

		history[item.exe][item.name].push({
			date: item.date,
			duration: item.duration
		});
	}

	console.log('Saving persistance ... ' + (new Date()).toISOString());

	await AppDataManager.saveObject('focus-stats', 'history', history);
	persistCache();

	console.log('Saved persistance ! ' + (new Date()).toISOString());

	console.log('Persisted, length:' + historyCache.length);
	console.log('Cache after persistance, length:' + historyCache.length);
}

setInterval(addNewActivityToHistoryCache, 1000);

setInterval(persistCache, 30 * 1000);
setInterval(persistFullData, 60 * 60 * 1000);
