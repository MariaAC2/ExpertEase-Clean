import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoBubbleComponent } from './photo-bubble.component';

describe('PhotoBubbleComponent', () => {
  let component: PhotoBubbleComponent;
  let fixture: ComponentFixture<PhotoBubbleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoBubbleComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoBubbleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
