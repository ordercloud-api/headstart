/* * * Logging Helpers used by cli-scripts * * */
const chalk = require('chalk')

module.exports = {
	cLog: function(message, code) {
		console.log(
			chalk.magentaBright('[TEST SCRIPTS] ') + chalk.cyanBright(message),
			code ? code : ''
		)
	},
	cSuccess: function(message, code) {
		console.log(
			chalk.magentaBright('[TEST SCRIPTS] ') + chalk.greenBright(message),
			code ? code : ''
		)
	},
	cError: function cError(message, code) {
		console.log(
			chalk.magentaBright('[TEST SCRIPTS] ') + chalk.redBright(message),
			code ? code : ''
		)
	},
}
