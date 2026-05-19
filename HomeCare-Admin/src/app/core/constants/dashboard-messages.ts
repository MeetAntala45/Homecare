import { SERVICE_PARTNER_MESSAGES } from "./service-partner-messages";

export const DashboardMessages = {
    EMPTY: {
        FUTURE: (label: string) => `No bookings yet for ${label}`,
        CURRENT: (label: string) => `No bookings yet for ${label}`,
        PAST: (label: string) => `No bookings were made in ${label}`,
    },
    FAIL: {
        CHART: "Failed to load chart",
        SERVICE_PARTNER : 'Failed to load partners'
    },
    SERVER:{
        ERROR: "Server Error"
    }
};