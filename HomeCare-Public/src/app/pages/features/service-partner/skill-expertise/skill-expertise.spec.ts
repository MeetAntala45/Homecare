import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SkillExpertise } from './skill-expertise';

describe('SkillExpertise', () => {
  let component: SkillExpertise;
  let fixture: ComponentFixture<SkillExpertise>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkillExpertise]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SkillExpertise);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
