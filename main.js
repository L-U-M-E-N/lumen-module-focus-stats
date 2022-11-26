import fs from 'fs';
import path from 'path';
import getActiveProgram from './getActiveProgram.cjs';

let history = {};
if(AppDataManager.exists('focus-stats', 'history')) {
	history = AppDataManager.loadObject('focus-stats', 'history');
}
if(!AppDataManager.exists('focus-stats', 'tags')) {
	AppDataManager.saveObject('focus-stats', 'tags', {});
}

let lastActivityExe = '';
let lastActivityName = '';
let lastActivityId = '';

const config = ConfigManager.get('focus-stats', 'cleaner');

function cleanString(str) {
	for(const value of config['keepEndOnly']) {
		if(str.endsWith(value)) {
			return value;
		}
	}

	for(const oldVal in config['substitutions']) {
		const newVal = config['substitutions'][oldVal];

		str = str.replace(oldVal, newVal);
	}

	return str.substring(0, 1000);
}

function addNewActivityToHistory() {
	const currProgramExe  = cleanString(getActiveProgram.getActiveProgramExe());
	const currProgramName = cleanString(getActiveProgram.getActiveProgramName());

	//console.log(new Date(), 'New activity:', currProgramName, getActiveProgram.getActiveProgramExe());
	//console.log(new Date(), 'Exe:', getActiveProgram.getActiveProgramExe());

	if(!history[currProgramExe]) {
		history[currProgramExe] = {};
	}
	if(!history[currProgramExe][currProgramName]) {
		history[currProgramExe][currProgramName] = [];
	}

	if(lastActivityExe === currProgramExe && lastActivityName === currProgramName) {
		history[currProgramExe][currProgramName][lastActivityId].duration++;
	} else {
		lastActivityExe  = currProgramExe;
		lastActivityName = currProgramName;

		lastActivityId = history[currProgramExe][currProgramName].push({
			date: new Date(),
			duration: 0,
		}) - 1;
	}

}

setInterval(addNewActivityToHistory, 1000);

setInterval(() => {
	AppDataManager.saveObject('focus-stats', 'history', history);
}, 30000);
