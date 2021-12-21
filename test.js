import getActiveProgram from './getActiveProgram.cjs';

setInterval(() => {
	console.log('exe:', getActiveProgram.getActiveProgramExe());
	console.log('name:', getActiveProgram.getActiveProgramName());
}, 1000);