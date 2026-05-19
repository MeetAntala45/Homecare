export const PartnerDashboardMessages = {
  EMPTY: {
    FUTURE: (label: string) => `No bookings yet for ${label}`,
    CURRENT: (label: string) => `No bookings yet for ${label}`,
    PAST: (label: string) => `No bookings were made in ${label}`,
  },
  FAIL: {
    CHART: 'No Data Found',
    METRICS: 'No Data Found',
  },
  SERVER: {
    ERROR: 'Server Error',
  },
};
