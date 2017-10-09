import { ClientWebApplicationPage } from './app.po';

describe('client-web-application App', () => {
  let page: ClientWebApplicationPage;

  beforeEach(() => {
    page = new ClientWebApplicationPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
