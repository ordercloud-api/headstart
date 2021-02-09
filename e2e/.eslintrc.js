module.exports = {
	'env': {
		'browser': true,
		'node': true,
		'es6': true,
	},
	'extends': [
		'eslint:recommended',
		'prettier/@typescript-eslint',
		'plugin:@typescript-eslint/recommended',
		'plugin:prettier/recommended',
	],
	'parser': '@typescript-eslint/parser',
	'parserOptions': {
		'ecmaFeatures': {
			'jsx': true,
		},
		'ecmaVersion': 2018,
		'sourceType': 'module',
	},
	'plugins': ['@typescript-eslint', 'prettier'],
	// javascript-specific overrides
	'overrides': [
		{
			'files': ['*.js'],
			'rules': {
				'@typescript-eslint/no-var-requires': 'off',
			},
		},
	],
	// custom rules go here
	'rules': {
		// found to conflict with prettier (run npm run eslint-check)
		'@typescript-eslint/member-delimiter-style': 'off',
		'@typescript-eslint/type-annotation-spacing': 'off',

		// kind of noisy and low value
		'@typescript-eslint/explicit-function-return-type': 'off',
		'@typescript-eslint/no-use-before-define': 'off',
		'@typescript-eslint/no-explicit-any': 'off',
		'@typescript-eslint/class-name-casing': 'off',
	},
	'globals': {
		'fixture': true,
		'test': true,
	},
}