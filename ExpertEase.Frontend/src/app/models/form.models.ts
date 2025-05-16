export interface FormField {
  key: string;            // formControlName
  label: string;
  type: string;           // 'text' | 'email' | 'password' | 'select' | etc.
  placeholder?: string;
  options?: string[];     // for select, radio, etc.
  required?: boolean;
}

export const DefaultLabelMap: Record<string, string> = {
  firstName: 'Prenume',
  lastName: 'Nume',
  email: 'Adresă e-mail',
  password: 'Parolă',
  role: 'Rol',
  phoneNumber: 'Număr de telefon',
  address: 'Adresă',
  yearsExperience: 'Ani experiență',
  description: 'Descriere',
  id: 'ID',
  account: 'Cont',
  transaction: 'Tranzacție',
  createdAt: 'Creat la',
  updatedAt: 'Actualizat la',
};

export function dtoToFormFields<T extends Record<string, any>>(
  dto: T,
  overrides: Partial<Record<keyof T, Partial<FormField> & { options?: string[] }>> = {},
  labelMap: Partial<Record<string, string>> = DefaultLabelMap
): FormField[] {
  return Object.keys(dto).map((key) => {
    const override = overrides[key as keyof T] || {};
    const type = override.type || (override.options ? 'select' : 'text');
    const label = labelMap[key] || prettifyLabel(key);

    return {
      key,
      label,
      type,
      placeholder: override.placeholder || `Introdu ${label.toLowerCase()}`,
      required: override.required ?? true,
      ...override
    };
  });
}

function prettifyLabel(key: string): string {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, s => s.toUpperCase());
}

export function dtoToDictionary(dto: Record<string, any>): Record<string, any> {
  if (!dto) return {};
  const labeledDict: Record<string, any> = {};
  const keys = Object.keys(dto);

  const hasFirstName = 'firstName' in dto;
  const hasLastName = 'lastName' in dto;

  // 1. Add ID first (always)
  if ('id' in dto) {
    const idLabel = DefaultLabelMap['id'] || 'id';
    labeledDict[idLabel] = dto['id'];
  }

  // 2. Add full name if both are present
  if (hasFirstName && hasLastName) {
    const fullName = `${dto['lastName']} ${dto['firstName']}`.trim();
    if (fullName) {
      labeledDict['Nume'] = fullName;
    }
  }

  // 3. Loop through rest
  for (const key of keys) {
    if (['id', 'firstName', 'lastName'].includes(key)) continue;

    const rawValue = dto[key];
    if (
      rawValue === null ||
      rawValue === undefined ||
      (typeof rawValue === 'string' && rawValue.trim() === '')
    ) {
      continue; // skip empty/null/blank
    }

    const label = DefaultLabelMap[key] || key;
    let formattedValue: any = rawValue;

    if (typeof rawValue === 'string' && isISODate(rawValue)) {
      console.log('ISO date detected:', rawValue);
      formattedValue = formatDate(rawValue);
    } else if (rawValue instanceof Date) {
      formattedValue = formatDate(rawValue);
    } else if (key.toLowerCase().includes('password')) {
      formattedValue = '********';
    }

    labeledDict[label] = formattedValue;
  }

  return labeledDict;
}

// Helper: Check if string is ISO date
function isISODate(value: string): boolean {
  return /^\d{4}-\d{2}-\d{2}T/.test(value);
}

// Helper: Format to DD.MM.YYYY
function formatDate(input: string | Date): string {
  const date = new Date(input);
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  return `${day}.${month}.${year}`;
}

