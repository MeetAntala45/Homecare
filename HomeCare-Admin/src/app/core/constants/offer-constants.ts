export const OPERATOR_OPTIONS = [
    { label: 'Greater than or equal (≥)', value: 'gte' },
    { label: 'Less than or equal (≤)', value: 'lte' },
    { label: 'Equal to (=)', value: 'eq' },
    { label: 'One of (in)', value: 'in' },
    { label: 'Between', value: 'between' },
]

export const FAIL_BEHAVIOUR_OPTIONS = [
    { label: 'Disable', value: 'disable' },
    { label: 'Hide', value: 'hide' },
]

export const INPUT_TYPE_OPTIONS = [
    { label: 'Number', value: 'number' },
    { label: 'Text', value: 'text' },
    { label: 'Time', value: 'time' },
    { label: 'Date', value: 'date' },
    { label: 'Days (comma separated)', value: 'days' },
    { label: 'Service Subcategory', value: 'subcategory' }
]

export const OPERATOR_LABEL_MAP: Record<string, string> = {
    gte: '≥ Greater than or equal',
    lte: '≤ Less than or equal',
    eq: '= Equal to',
    in: 'One of',
    between: 'Between',
};

export const INPUT_TYPE_OPERATORS: Record<string, string[]> = {
    number: ['gte', 'lte', 'eq', 'between'],
    date: ['eq', 'between'],
    time: ['between'],
    days: ['in'],
    text: ['eq', 'in'],
    subcategory: ['in'],
};

export const CONTEXT_KEY_INPUT_TYPES: Record<string, string[]> = {
    cart_total:         ['number'],
    user_booking_count: ['number'],
    user_coupon_uses:   ['number'],
    slot_day_of_week:   ['days'],
    slot_time:          ['time'],
    slot_date:          ['date'],
    service_sub_category_id: ['subcategory'],
  };