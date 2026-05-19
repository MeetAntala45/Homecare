import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerUser } from './customer-user';

describe('CustomerUser', () => {
  let component: CustomerUser;
  let fixture: ComponentFixture<CustomerUser>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomerUser]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerUser);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
