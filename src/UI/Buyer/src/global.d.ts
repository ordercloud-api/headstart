type eventTypes = 'trackEvent' | 'blah'

interface Window {
  rfk: {
    uid: () => string
    push: ([eventTypes, any]) => void
  }
}
