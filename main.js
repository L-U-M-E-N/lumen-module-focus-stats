const fs = require('fs');
const path = require('path');
const getActiveProgram = require('./build/Release/getActiveProgram');

let history = [];
if(AppDataManager.exists('focus-stats', 'history')) {
	history = AppDataManager.loadObject('focus-stats', 'history');
}

function addNewActivityToHistory() {
	const currProgramName = getActiveProgram.getActiveProgramName();

	console.log(new Date(), 'New activity:', currProgramName, getActiveProgram.getActiveProgramExe());
	console.log(new Date(), 'Exe:', getActiveProgram.getActiveProgramExe());

	history.push({
		exe: getActiveProgram.getActiveProgramExe(),
		name: currProgramName,
		date: new Date(),
		duration: 0,
	});
}
addNewActivityToHistory();

setInterval(() => {
	if(history[history.length - 1].name === getActiveProgram.getActiveProgramName()) {
		history[history.length - 1].duration++;
	} else {
		addNewActivityToHistory();
	}

	AppDataManager.saveObject('focus-stats', 'history', history);
}, 1000);