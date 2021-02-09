import { TestBed, inject } from '@angular/core/testing';

import { RegexService } from '@app-seller/shared/services/regex/regex.service';

describe('RegexService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [],
    });
  });

  it('should be created', inject([RegexService], (service: RegexService) => {
    expect(service).toBeTruthy();
  }));
});
