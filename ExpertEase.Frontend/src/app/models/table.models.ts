export interface TableColumn {
  key: string;           // property name in the object
  header: string;        // column header label
  type?: 'text' | 'action'; // optionally allow action column type
  compute?: (row: any) => string;
  actions?: TableAction[];  // for action columns
}

export interface TableAction {
  label: string;
  icon?: string;
  callback: (row: any) => void;
  class?: string; // e.g. 'edit', 'delete'
}
