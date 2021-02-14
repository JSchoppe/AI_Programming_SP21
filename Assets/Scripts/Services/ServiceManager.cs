using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Handles single instance services.
    /// </summary>
    public static class ServiceManager
    {
        #region Services Collection Field
        private static List<object> services;
        #endregion
        #region Static Initializer
        static ServiceManager()
        {
            services = new List<object>();
        }
        #endregion
        #region Service Methods
        /// <summary>
        /// Retrieves the requested service, initializing it if not yet created.
        /// </summary>
        /// <typeparam name="T">The service to retrieve.</typeparam>
        /// <returns>The requested service.</returns>
        public static T RetrieveService<T>()
            where T : new()
        {
            // Check if the service already exists.
            foreach (T service in services)
                return service;
            // If not make a new instance of the service.
            T newService = new T();
            services.Add(newService);
            return newService;
        }
        #endregion
    }
}
