using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IWindowService
    {
        public void Show(Type windowType);

        public T Show<T>() where T : class;
    }
}

