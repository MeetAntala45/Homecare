import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfessionalEntry } from './professional-entry';

describe('ProfessionalEntry', () => {
  let component: ProfessionalEntry;
  let fixture: ComponentFixture<ProfessionalEntry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfessionalEntry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProfessionalEntry);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
