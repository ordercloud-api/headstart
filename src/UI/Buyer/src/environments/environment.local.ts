/* eslint-disable @typescript-eslint/no-unsafe-assignment */

// ===== MAKE CHANGES TO CONFIGURATION BETWEEN THESE LINES ONLY =======
// ====================================================================
const brand = Brand.DEFAULT_BUYER
const appEnvironment = Environment.TEST
const useLocalMiddleware = false
const localMiddlewareURL = 'https://localhost:44304'
// ====================================================================
// ======= UNLESS YOU ARE DOING SOMETHING WEIRD =======================

import defaultbuyertest from '../assets/appConfigs/defaultbuyer-test.json'
import defaultbuyeruat from '../assets/appConfigs/defaultbuyer-uat.json'
import defaultbuyerproduction from '../assets/appConfigs/defaultbuyer-production.json'

const apps = {
  TEST: {
    DEFAULT_BUYER: defaultbuyertest,
  },
  UAT: {
    DEFAULT_BUYER: defaultbuyeruat,
  },
  PRODUCTION: {
    DEFAULT_BUYER: defaultbuyerproduction,
  },
}

// for easier debugging in development mode, ignores zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
import 'zone.js/dist/zone-error'
import {
  Brand,
  Environment,
  EnvironmentConfig,
} from 'src/app/models/environment.types'

const target = apps[appEnvironment][brand] as EnvironmentConfig
target.hostedApp = false
target.appInsightsInstrumentationKey = ''
if (useLocalMiddleware) {
  target.middlewareUrl = localMiddlewareURL
}
export const environment = target
