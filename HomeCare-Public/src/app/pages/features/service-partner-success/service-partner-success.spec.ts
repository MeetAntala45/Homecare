import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServicePartnerSuccess } from './service-partner-success';

describe('ServicePartnerSuccess', () => {
  let component: ServicePartnerSuccess;
  let fixture: ComponentFixture<ServicePartnerSuccess>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServicePartnerSuccess]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServicePartnerSuccess);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
