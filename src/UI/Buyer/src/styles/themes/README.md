# Theme

Want to add a new theme?

1. Copy `defaultbuyer` folder and rename it to the first part of your appID. For example if your appID is `catstore-test` then the folder should be called `catstore`
2. In angular.json (at the root of the project) under the deploy configuration add a new entry to the styles array. Make sure the bundleName is the same as the folder name