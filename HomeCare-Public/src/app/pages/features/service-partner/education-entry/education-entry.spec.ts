import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EducationEntry } from './education-entry';

describe('EducationEntry', () => {
  let component: EducationEntry;
  let fixture: ComponentFixture<EducationEntry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EducationEntry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EducationEntry);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
