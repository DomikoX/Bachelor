using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NLog;

namespace RemoteWcfService
{
    public class GlobalErrorHandler : IErrorHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public bool HandleError(Exception error)
        {
            Logger.Error($"{error.Message} || {error.Source} || {error.StackTrace}");
            return true;
        }

        public void ProvideFault(Exception error,
            System.ServiceModel.Channels.MessageVersion version,
            ref System.ServiceModel.Channels.Message fault)
        {
            var newEx = new FaultException($"Exception caught at Service Application GlobalErrorHandler{ Environment.NewLine }Method: { error.TargetSite.Name }{ Environment.NewLine}Message: { error.Message}");

            MessageFault msgFault = newEx.CreateMessageFault();
            fault = Message.CreateMessage(version, msgFault, newEx.Action);
        }
    }


    public class GlobalErrorBehaviorAttribute : Attribute, IServiceBehavior
    {
        private readonly Type _errorHandlerType;

        /// <summary>
        /// Dependency injection to dynamically inject error handler 
        /// if we have multiple global error handlers
        /// </summary>
        /// <param name="errorHandlerType"></param>
        public GlobalErrorBehaviorAttribute(Type errorHandlerType)
        {
            this._errorHandlerType = errorHandlerType;
        }

        #region IServiceBehavior Members

        void IServiceBehavior.Validate(ServiceDescription description,
            ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection parameters)
        {
        }

        /// <summary>
        /// Registering the instance of global error handler in 
        /// dispatch behavior of the service
        /// </summary>
        /// <param name="description"></param>
        /// <param name="serviceHostBase"></param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,
            ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandler;

            try
            {
                errorHandler = (IErrorHandler) Activator.CreateInstance(_errorHandlerType);
            }
            catch (MissingMethodException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must have apublic empty constructor.", e);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must implement System.ServiceModel.Dispatcher.IErrorHandler.", e);
            }

            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }

        #endregion IServiceBehavior Members
    }
}