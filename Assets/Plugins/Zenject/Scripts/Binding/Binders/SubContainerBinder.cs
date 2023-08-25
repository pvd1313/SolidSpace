using System;

namespace Zenject
{
    [NoReflectionBaking]
    public class SubContainerBinder
    {
        readonly BindInfo _bindInfo;
        readonly BindStatement _bindStatement;
        readonly object _subIdentifier;
        readonly bool _resolveAll;

        public SubContainerBinder(
            BindInfo bindInfo,
            BindStatement bindStatement,
            object subIdentifier, bool resolveAll)
        {
            _bindInfo = bindInfo;
            _bindStatement = bindStatement;
            _subIdentifier = subIdentifier;
            _resolveAll = resolveAll;

            // Reset in case the user ends the binding here
            bindStatement.SetFinalizer(null);
        }

        protected IBindingFinalizer SubFinalizer
        {
            set { _bindStatement.SetFinalizer(value); }
        }

        public ScopeConcreteIdArgConditionCopyNonLazyBinder ByInstance(DiContainer subContainer)
        {
            SubFinalizer = new SubContainerBindingFinalizer(
                _bindInfo, _subIdentifier, _resolveAll,
                (_) => new SubContainerCreatorByInstance(subContainer));

            return new ScopeConcreteIdArgConditionCopyNonLazyBinder(_bindInfo);
        }

        public ScopeConcreteIdArgConditionCopyNonLazyBinder ByInstanceGetter(
            Func<InjectContext, DiContainer> subContainerGetter)
        {
            SubFinalizer = new SubContainerBindingFinalizer(
                _bindInfo, _subIdentifier, _resolveAll,
                (_) => new SubContainerCreatorByInstanceGetter(subContainerGetter));

            return new ScopeConcreteIdArgConditionCopyNonLazyBinder(_bindInfo);
        }

        public
#if NOT_UNITY3D
            WithKernelScopeConcreteIdArgConditionCopyNonLazyBinder
#else
            WithKernelDefaultParentScopeConcreteIdArgConditionCopyNonLazyBinder
#endif
            ByInstaller<TInstaller>()
            where TInstaller : InstallerBase
        {
            return ByInstaller(typeof(TInstaller));
        }

        public
#if NOT_UNITY3D
            WithKernelScopeConcreteIdArgConditionCopyNonLazyBinder
#else
            WithKernelDefaultParentScopeConcreteIdArgConditionCopyNonLazyBinder
#endif
            ByInstaller(Type installerType)
        {
            Assert.That(installerType.DerivesFrom<InstallerBase>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'Installer<>'", installerType);

            var subContainerBindInfo = new SubContainerCreatorBindInfo();

            SubFinalizer = new SubContainerBindingFinalizer(
                _bindInfo, _subIdentifier, _resolveAll,
                (container) => new SubContainerCreatorByInstaller(container, subContainerBindInfo, installerType));

            return new
#if NOT_UNITY3D
                WithKernelScopeConcreteIdArgConditionCopyNonLazyBinder
#else
                WithKernelDefaultParentScopeConcreteIdArgConditionCopyNonLazyBinder
#endif
                (subContainerBindInfo, _bindInfo);
        }

        public
#if NOT_UNITY3D
            WithKernelScopeConcreteIdArgConditionCopyNonLazyBinder
#else
            WithKernelDefaultParentScopeConcreteIdArgConditionCopyNonLazyBinder
#endif
            ByMethod(Action<DiContainer> installerMethod)
        {
            var subContainerBindInfo = new SubContainerCreatorBindInfo();

            SubFinalizer = new SubContainerBindingFinalizer(
                _bindInfo, _subIdentifier, _resolveAll,
                (container) => new SubContainerCreatorByMethod(container, subContainerBindInfo, installerMethod));

            return new
#if NOT_UNITY3D
                WithKernelScopeConcreteIdArgConditionCopyNonLazyBinder
#else
                WithKernelDefaultParentScopeConcreteIdArgConditionCopyNonLazyBinder
#endif
                (subContainerBindInfo, _bindInfo);
        }

#if !NOT_UNITY3D
#endif
    }
}
