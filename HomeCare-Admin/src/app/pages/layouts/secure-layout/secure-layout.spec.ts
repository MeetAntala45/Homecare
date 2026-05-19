import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SecureLayout } from './secure-layout';

describe('SecureLayout', () => {
  let component: SecureLayout;
  let fixture: ComponentFixture<SecureLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SecureLayout]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SecureLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
