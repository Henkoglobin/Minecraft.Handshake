using System;

namespace Minecraft.BuildingBlocks.DependencyInjection {
	internal class ActivatorServiceProvider : IServiceProvider {
		public object GetService(Type serviceType) 
			=> Activator.CreateInstance(serviceType);
	}
}
