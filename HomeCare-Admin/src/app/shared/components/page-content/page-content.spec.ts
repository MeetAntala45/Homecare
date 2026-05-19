import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageContent } from './page-content';

describe('PageContent', () => {
  let component: PageContent;
  let fixture: ComponentFixture<PageContent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageContent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PageContent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
