using System;
using GraphQL.Caching;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQL.Types.Relay;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;

namespace GraphQL.DI
{
    /// <summary>
    /// Base implementation of <see cref="IGraphQLBuilder"/>.
    /// </summary>
    public abstract class GraphQLBuilderBase : IGraphQLBuilder
    {
        /// <summary>
        /// Register the default services required by GraphQL if they have not already been registered.
        /// Includes graph types required for connection builders (GraphQL Relay) and generic graph types
        /// such as <see cref="EnumerationGraphType{TEnum}"/> and <see cref="AutoRegisteringObjectGraphType{TSourceType}"/>.
        /// <br/><br/>
        /// Does not include <see cref="IDocumentWriter"/>, and the default <see cref="IDocumentExecuter"/>
        /// implementation does not support subscriptions.
        /// </summary>
        protected virtual void RegisterDefaultServices()
        {
            // configure an error to be displayed when no IDocumentWriter is registered
            Services.TryRegister<IDocumentWriter>(_ =>
            {
                throw new InvalidOperationException(
                    "IDocumentWriter not set in DI container. " +
                    "Add a IDocumentWriter implementation, for example " +
                    "GraphQL.SystemTextJson.DocumentWriter or GraphQL.NewtonsoftJson.DocumentWriter. " +
                    "For more information, see: https://github.com/graphql-dotnet/graphql-dotnet/blob/master/README.md and https://github.com/graphql-dotnet/server/blob/develop/README.md.");
            }, ServiceLifetime.Transient);

            // configure service implementations to use the configured default services when not overridden by a user
            Services.TryRegister<IDocumentExecuter, DocumentExecuter>(ServiceLifetime.Singleton);
            Services.TryRegister<IDocumentBuilder, GraphQLDocumentBuilder>(ServiceLifetime.Singleton);
            Services.TryRegister<IDocumentValidator, DocumentValidator>(ServiceLifetime.Singleton);
            Services.TryRegister<IComplexityAnalyzer, ComplexityAnalyzer>(ServiceLifetime.Singleton);
            Services.TryRegister<IDocumentCache>(DefaultDocumentCache.Instance);
            Services.TryRegister<IErrorInfoProvider, ErrorInfoProvider>(ServiceLifetime.Singleton);

            // configure relay graph types
            Services.TryRegister(typeof(EdgeType<>), typeof(EdgeType<>), ServiceLifetime.Transient);
            Services.TryRegister(typeof(ConnectionType<>), typeof(ConnectionType<>), ServiceLifetime.Transient);
            Services.TryRegister(typeof(ConnectionType<,>), typeof(ConnectionType<,>), ServiceLifetime.Transient);
            Services.TryRegister<PageInfoType>(ServiceLifetime.Transient);

            // configure generic graph types
            Services.TryRegister(typeof(EnumerationGraphType<>), typeof(EnumerationGraphType<>), ServiceLifetime.Transient);
            Services.TryRegister(typeof(InputObjectGraphType<>), typeof(InputObjectGraphType<>), ServiceLifetime.Transient);
            Services.TryRegister(typeof(AutoRegisteringInputObjectGraphType<>), typeof(AutoRegisteringInputObjectGraphType<>), ServiceLifetime.Transient);
            Services.TryRegister(typeof(AutoRegisteringObjectGraphType<>), typeof(AutoRegisteringObjectGraphType<>), ServiceLifetime.Transient);

            // configure execution to use the default registered schema if none specified
            this.ConfigureExecutionOptions(options =>
            {
                if (options.RequestServices != null && options.Schema == null)
                {
                    options.Schema = options.RequestServices.GetService(typeof(ISchema)) as ISchema;
                }
            });

            // configure mapping for IOptions<ErrorInfoProviderOptions>
            Services.Configure<ErrorInfoProviderOptions>();
        }

        /// <inheritdoc />
        public abstract IServiceRegister Services { get; }
    }
}
