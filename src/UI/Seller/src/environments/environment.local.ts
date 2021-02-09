// ===== MAKE CHANGES TO CONFIGURATION BETWEEN THESE LINES ONLY =======
// ====================================================================
const brand = Brand.DEFAULT_ADMIN
const sebEnvironment = Environment.TEST
const useLocalMiddleware = false
const localMiddlewareURL = 'https://localhost:5001'
// ====================================================================
// ======= UNLESS YOU ARE DOING SOMETHING WEIRD =======================

import defaultadmintest from '../assets/appConfigs/defaultadmin-test.json'
import defaultadminstaging from '../assets/appConfigs/defaultadmin-staging.json'
import defaultadminproduction from '../assets/appConfigs/defaultadmin-production.json'

const apps = {
  TEST: {
    DEFAULT_ADMIN: defaultadmintest,
  },
  UAT: {
    DEFAULT_ADMIN: defaultadminstaging,
  },
  PRODUCTION: {
    DEFAULT_ADMIN: defaultadminproduction,
  },
}

// for easier debugging in development mode, ignores zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
import 'zone.js/dist/zone-error'
import {
  Brand,
  Environment,
  EnvironmentConfig,
} from '@app-seller/models/environment.types'
const target: EnvironmentConfig = apps[sebEnvironment][brand]
target.hostedApp = false
if (useLocalMiddleware) {
  target.middlewareUrl = localMiddlewareURL
}
export const environment = target
