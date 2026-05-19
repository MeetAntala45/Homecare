import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceListingPage } from './service-listing-page';

describe('ServiceListingPage', () => {
  let component: ServiceListingPage;
  let fixture: ComponentFixture<ServiceListingPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServiceListingPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServiceListingPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
