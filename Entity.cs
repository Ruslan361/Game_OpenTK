using System;
using System.Collections.Generic;

namespace Simple3DGame {
    class Entity {
            private Dictionary<Type, Component> components = new();

            public void AddComponent<T>(T component) where T : Component {
                components[typeof(T)] = component;
            }

            public T GetComponent<T>() where T : Component {
                return (T)components[typeof(T)];
            }

            public bool TryGetComponent<T>(out T component) where T : Component {
                if (components.TryGetValue(typeof(T), out var comp)) {
                    component = (T)comp;
                    return true;
                }
                component = default!; // Используем default! вместо null
                return false;
            }
        }
}