import { ShipMethodNameMapperPipe } from "./ship-method-name.pipe";

describe('ShipMethodNameMapperPipe', () => {
    // This pipe is a pure, stateless function so no need for BeforeEach
    const pipe = new ShipMethodNameMapperPipe();
  
    it('transforms "FEDEX_GROUND" to "Ground"', () => {
      expect(pipe.transform('FEDEX_GROUND')).toBe('Ground');
    });
  
    it('transforms "null" to ""', () => {
      expect(pipe.transform(null)).toBe('');
    });

    it('transforms "NON_EXISTENT_METHOD" to "NON_EXISTENT_METHOD"', () => {
      expect(pipe.transform('NON_EXISTENT_METHOD')).toBe('NON_EXISTENT_METHOD');
    });
  });