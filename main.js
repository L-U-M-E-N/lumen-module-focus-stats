const fs = require('fs');
const path = require('path');
const getActiveProgram = require('./build/Release/getActiveProgram');

const historyJson = fs.readFileSync(path.resolve(__dirname, './history.json'), 'utf-8');
const history = JSON.parse(historyJson);

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

	fs.writeFileSync(path.resolve(__dirname, './history.json'), JSON.stringify(history));
}, 1000);