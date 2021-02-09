import { BaseBrowse } from '@app-seller/shared/models/base-browse.class';

class TestBaseBrowse extends BaseBrowse {
  loadData() {}
}

const component = new TestBaseBrowse();

describe('BaseBrowse', () => {
  beforeEach(() => {
    spyOn(component, 'loadData');
  });
  it('should search, reseting page', () => {
    component.requestOptions = { page: 2, search: 'hose', sortBy: 'ID' };
    component.searchChanged('newSearch');
    expect(component.requestOptions).toEqual({
      page: undefined,
      search: 'newSearch',
      sortBy: 'ID',
    });
  });
  it('should change page, keeping search + sort', () => {
    component.requestOptions = { page: 2, search: 'hose', sortBy: 'ID' };
    component.pageChanged(3);
    expect(component.requestOptions).toEqual({
      page: 3,
      search: 'hose',
      sortBy: 'ID',
    });
  });
  it('should sort, resting page ', () => {
    component.requestOptions = { page: 2, search: 'hose', sortBy: 'ID' };
    component.sortChanged('!ID');
    expect(component.requestOptions).toEqual({
      page: undefined,
      search: 'hose',
      sortBy: '!ID',
    });
  });
});
