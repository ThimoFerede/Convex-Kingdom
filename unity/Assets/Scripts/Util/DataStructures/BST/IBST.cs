﻿namespace Util.DataStructures.BST
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Generic interface for a binary search tree (BST).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBST<T> where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Check whether the tree contains the given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Contains(T data);

        /// <summary>
        /// Count the number of nodes in the tree.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Inserts a new data value into the tree.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>whether the insertion was succesful.</returns>
        bool Insert(T data);

        /// <summary>
        /// Finds the maximum value in the tree and sets it to the out variable.
        /// </summary>
        /// <param name="out_MaxValue"></param>
        /// <returns>whether the maximum was found</returns>
        bool FindMax(out T out_MaxValue);

        /// <summary>
        /// Finds the minimum value in the tree and sets it to the out variable.
        /// </summary>
        /// <param name="out_MaxValue"></param>
        /// <returns>whether the minimum was found</returns>
        bool FindMin(out T out_MinValue);

        /// <summary>
        /// Delete the given data value from the tree.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>whether the deletion was succeful.</returns>
        bool Delete(T data);

        /// <summary>
        /// Deletes the maximum value from the tree.
        /// </summary>
        /// <returns>the given maximum value that was removed</returns>
        T DeleteMax();

        /// <summary>
        /// Deletes the maximum value from the tree.
        /// </summary>
        /// <returns>the given maximum value that was removed</returns>
        T DeleteMin();

        /// <summary>
        /// Clears the tree of all nodes.
        /// </summary>
        void Clear();
    }
}
