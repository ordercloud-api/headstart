# Contributing to ordercloud-javascript-sdk

Hello! Thanks for your interest in contributing! Before implementing new features and changes please create an issue so we can have a discussion about it!

## ⌨ Writing code

Unlike other libraries, most of the code here is not handwritten. We actually use [@ordercloud/oc-codegen](https://github.com/ordercloud-api/oc-codegen) to generate the code based on a set of templates located in `/codegen/`. This tool uses OrderCloud's OpenAPI specification along with the templates to generate the full SDK. That means most of your time should be spent in the `/codegen/` folder. Any direct changes in `/src/` will simply be overwritten when you run `yarn build`.

## Template Data

The source of the data comes from the open api spec but then the oc-codegen library massages the data to make it easier to work with. That library exposes "hooks" which allow us to step in and do our own processind during that data massage layer. In our case we add javascript/typescript specific properties that allow us to build a rich SDK. All of our hooks are available in a single file [codegen/hooks.ts](../codegen/hooks.ts)

To view the template data simply run the following command which will output the data to `templateData.json` at the root of the project.

```shell
yarn debug-template-data
```

## Generating the SDK

After updating the templates/hooks you can generate the code which will be output in the `src/` directory

```shell
yarn generate-sdk
```

## ✨ Submitting a pull request

1. Fork this repository
2. Create a new branch with the name of the feature you plan to work on
3. Install dependencies using [yarn](https://yarnpkg.com/en/)
4. Make your changes. Note: all of your changes should be to files in `/codegen` and not directly to any of the files in `src/` as those files get overwritten when the code is generated.
5. Run `yarn build` to regenerate the sdk, compile the code, run the tests, and generate the docs
6. Verify your changes work as expected. Run `npm install /path/to/this/folder` in a different project to install locally and test
7. Update the [changelog](../CHANGELOG.md)
8. Commit your changes. We adhere to the [gitmoji](https://github.com/carloscuesta/gitmoji/) standard
9. Update the sdk version in the [package.json](../package.json) and make a separate version commit in the format `:bookmark: v{newVersionHere}`
10. Create a pull request
11. Have a beer! 🍺

## 🚀 Releasing

Assuming you or a contributor followed the instructions for [submitting a pull request](#✨-submitting-a-pull-request) and are a maintainer you can follow these instructions to release a new version of the sdk.

1. Verify the version has been bumped and adheres to [semantic versioning](https://semver.org/)
2. Verify the [changelog](../CHANGELOG.md) has been updated
3. Create a publish a new release on github
4. Publish on npm by running `npm run publish-please`
   - Defaults to pre-release. It is recommended to do a pre-release first to vet changes
   - For normal releases update the `publishTag` in `.publishrc` to `latest` and then run `npm run publish-please`
5. Have a beer! 🍻
