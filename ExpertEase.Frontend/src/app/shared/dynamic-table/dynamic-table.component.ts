import {Component, EventEmitter, Input, Output} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableColumn } from '../../models/table.models';
import {FormsModule} from '@angular/forms'; // adjust path

@Component({
  selector: 'app-dynamic-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dynamic-table.component.html',
  styleUrls: ['./dynamic-table.component.scss']
})
export class DynamicTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
}
