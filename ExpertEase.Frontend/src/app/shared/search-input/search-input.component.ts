import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-search-input',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-input.component.html',
  styleUrl: './search-input.component.scss'
})
export class SearchInputComponent {
  searchTerm: string = '';

  @Input() placeholder: string = 'Caută ...';
  @Output() searchChanged = new EventEmitter<string>();
  onSearchChanged() {
    this.searchChanged.emit(this.searchTerm.trim());
  }
}
