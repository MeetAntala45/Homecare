import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlockConfirmation } from './block-confirmation';

describe('BlockConfirmation', () => {
  let component: BlockConfirmation;
  let fixture: ComponentFixture<BlockConfirmation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BlockConfirmation]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BlockConfirmation);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
