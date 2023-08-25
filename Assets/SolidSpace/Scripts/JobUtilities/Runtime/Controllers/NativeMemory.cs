using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace SolidSpace.JobUtilities
{
    public static class NativeMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateTempArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreatePermArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> CreatePersistentReference<T>(T withValue) where T : unmanaged
        {
            var reference = new NativeReference<T>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            reference.Value = withValue;
            return reference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> CreateTempJobReference<T>() where T : unmanaged
        {
            return new NativeReference<T>(Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref NativeArray<T> permArray, int requiredCapacity)
            where T : unmanaged
        {
            if (permArray.Length >= requiredCapacity)
            {
                return;
            }

            var newSize = 1 << (int) Math.Ceiling(Math.Log(requiredCapacity, 2));
            var newArray = CreatePermArray<T>(newSize);
            NativeArray<T>.Copy(permArray, newArray, permArray.Length);
            
            permArray.Dispose();
            permArray = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MaintainPersistentArrayLength<T>(ref NativeArray<T> array, ArrayMaintenanceData rule) 
            where T : unmanaged
        {
            var requiredCapacity = rule.requiredCapacity;
            if (array.Length >= requiredCapacity)
            {
                return;
            }
            
            var itemsPerAllocation = rule.itemPerAllocation;
            var chunkBasedLength = (int) Math.Ceiling(requiredCapacity / (float) itemsPerAllocation) * itemsPerAllocation;
            var newArray = CreatePermArray<T>(chunkBasedLength);

            if (rule.copyOnResize)
            {
                NativeArray<T>.Copy(array, newArray, array.Length);
            }
            
            array.Dispose();
            array = newArray;
        }
    }
}