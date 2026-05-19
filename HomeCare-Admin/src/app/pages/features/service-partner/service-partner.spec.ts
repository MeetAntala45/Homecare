import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServicePartner } from './service-partner';

describe('ServicePartner', () => {
  let component: ServicePartner;
  let fixture: ComponentFixture<ServicePartner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServicePartner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServicePartner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
