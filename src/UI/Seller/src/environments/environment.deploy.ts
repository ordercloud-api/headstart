/* eslint-disable @typescript-eslint/no-unsafe-assignment */

// in the release phase the token '#{environmentConfig}' is replaced by the correct app configuration
// this enables a "one build, multiple deploy" strategy
export const environment = '#{environmentConfig}' as any
