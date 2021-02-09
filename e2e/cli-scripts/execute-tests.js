const fs = require('fs')
const childProcess = require('child_process')
const logger = require('./logger')
const args = process.argv
const concurrency = args[2]
const browser = args[3]
const outputPath = args[4]
const fileName = args[5]
const testRun = args[6]

logger.cLog('Loading `execute tests` ')

if (!concurrency) {
	logger.cError('Please define concurrency for the test run')
	return
}

if (!browser) {
	logger.cError('Please define a browser for the test run')
	return
}

if (!outputPath) {
	logger.cError('Please define outputPath for the test report')
	return
}

if (!fileName) {
	logger.cError('Please define fileName for the test report')
	return
}
if (!testRun) {
	logger.cError('Please define TestRun number for the test execution')
	return
}

let runCount = 0

runTests()

function runTests() {
	if (runCount >= 4) {
		return
	}
	runCount++
	let readOutput
	const time = new Date()
	logger.cLog(
		'Start time: ' +
			time.getHours() +
			':' +
			time.getMinutes() +
			':' +
			time.getSeconds()
	)
	logger.cLog(`Starting test run #${runCount}...`)

	//create output file, or overwrite it if it already exists
	fs.writeFileSync(`${outputPath}/testOutput.txt`, '', function(err) {
		if (err) {
			logger.cError('Error creating file')
		}
	})

	//execute the TestCafe command that runs the tests
	const testOutput = childProcess.exec(
		`yarn testcafe -c ${concurrency} ${browser} tests/ --fixture-meta TestRun=${testRun} --page-load-timeout 60000 --assertion-timeout 60000 --selector-timeout 60000 --skip-js-errors -q -S -s ${outputPath} --reporter spec,xunit:${outputPath}/${fileName}.xml,html:${outputPath}/${fileName}.html,testcafe-to-testrail:${outputPath}/${fileName}.txt`
	)

	//write output to the output file
	testOutput.stdout.on('data', data => {
		fs.appendFileSync(`${outputPath}/testOutput.txt`, data, function(err) {
			if (err) {
				logger.cError('Error writing to output file')
			}
		})

		if (data == '\n') {
			process.stdout.write(data)
		} else {
			const time = new Date()
			process.stdout.write(
				time.getHours() +
					':' +
					time.getMinutes() +
					':' +
					time.getSeconds() +
					': ' +
					data
			)
		}
	})

	//write errors to the output file
	testOutput.stderr.on('data', data => {
		fs.appendFileSync(`${outputPath}/testOutput.txt`, data, function(err) {
			if (err) {
				logger.cError('Error writing to output file')
			}
		})

		const time = new Date()
		process.stdout.write(
			time.getHours() +
				':' +
				time.getMinutes() +
				':' +
				time.getSeconds() +
				': ' +
				data
		)
	})

	//when the command is complete, check if there is a TestCafe error in the output
	//If there is, then rerun the tests
	testOutput.on('exit', function() {
		readOutput = fs.readFileSync(`${outputPath}/testOutput.txt`, function(
			err
		) {
			if (err) {
				logger.cError('Error reading output file')
			}
		})
		//logger.cLog(readOutput)

		if (
			readOutput.includes(
				'Unable to establish one or more of the specified browser connections'
			)
		) {
			logger.cLog('Run failed with TestCafe error')
			runTests()
		}
	})
}
