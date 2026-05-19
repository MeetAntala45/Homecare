export const MASTER_DATA_MESSAGES = {
    category: {
        ADDED: 'Category Added Successfully',
        DELETED: 'Category Deleted Successfully',
        EXISTS: 'Category already exists.',
        HAS_SUBCATEGORY: 'This Category has Sub Categories',
        SAVE_FAILED: 'Failed to save category'
    },

    subcategory: {
        ADDED: 'SubCategory Added Successfully',
        DELETED: 'SubCategory Deleted Successfully',
        EXISTS: 'SubCategory already exists.',
        SAVE_FAILED: 'Failed to save subcategory'
    },

    serviceType : {
        HAS_CATEGORY : 'This service type has categories',
        FAIL: 'Failed to check categories.'
    },

    common: {
        SELECT_CATEGORY_FIRST: 'Please add or select Category first',
        RELOAD_FAILED: 'Failed to reload categories',
        DATA_ADDED: 'Data added Successfully',
        LOAD_FAILED : 'Failed to load service type',
    },
    conformation : {
        SERVICETYPE_DELETED: (serviceName : string )  => `Are you sure you want to delete <strong>${serviceName}</strong> Service Type?`,
        CATEGORY_DELETED: (serviceName : string )  => `Are you sure you want to delete <strong>${serviceName}</strong> Category?`,
        SUBCATEGORY_DELETED: (serviceName : string )  => `Are you sure you want to delete <strong>${serviceName}</strong> SubCategory?`,
        
    }
};