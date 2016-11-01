// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC
{
    /// <summary>
    /// Aids in sorting for the best ordering given dependencies.
    /// </summary>
    public class TopologicalSorter
    {
        #region Private Variables

        private int _numVerts;                  // current number of vertices  
        private readonly int[] _vertices;       // list of vertices  
        private readonly int[,] _matrix;        // adjacency matrix  
        private readonly int[] _sortedArray;    // sorted vert labels

        #endregion

        #region Constructor

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="size">The expected number of vertices that require sorting.</param>
        public TopologicalSorter(int size)
        {
            _vertices = new int[size];
            _matrix = new int[size, size];
            _numVerts = 0;
            
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    _matrix[i, j] = 0;
                }
            }

            _sortedArray = new int[size];
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a vertex to the lists for sorting.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int AddVertex(int vertex)
        {
            _vertices[_numVerts++] = vertex;
            return _numVerts - 1;
        }

        /// <summary>
        /// Adds an edge to indicate a dependency between two vertices.
        /// </summary>
        /// <param name="start">The starting vertex.</param>
        /// <param name="end">The dependent vertex.</param>
        public void AddEdge(int start, int end)
        {
            _matrix[start, end] = 1;
        }

        /// <summary>
        /// Perform a topological sort on the added vertices given their dependencies.
        /// </summary>
        /// <returns>The sorted array of vertices.</returns>
        public int[] Sort()  
        {
            // while vertices remain,
            while (_numVerts > 0)
            {
                // get a vertex with no successors, or -1  
                var currentVertex = NoSuccessors();

                if (currentVertex == -1)
                {
                    // must be a cycle
                    throw new Exception("Graph has cycles.");
                }

                // insert vertex label in sorted array (start at end)  
                _sortedArray[_numVerts - 1] = _vertices[currentVertex];

                // delete vertex
                DeleteVertex(currentVertex);
            }

            // vertices all gone; return sortedArray  
            return _sortedArray;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a vertex with no successors.
        /// </summary>
        /// <returns>The vertex found or -1 if there were none.</returns>
        private int NoSuccessors()
        {
            for (var row = 0; row < _numVerts; row++)
            {
                var edge = false;

                // edge from row to column in adjacency matrix
                for (var col = 0; col < _numVerts; col++)
                {
                    // if edge to another,
                    if (_matrix[row, col] > 0)
                    {
                        edge = true;

                        // this vertex has a successor try another
                        break;
                    }
                }

                // if no edges, has no successors 
                if (!edge)
                {
                    return row;
                }
            }

            // none
            return -1;
        }

        /// <summary>
        /// Deletes a vertex from the internal structures.
        /// </summary>
        /// <param name="vertex">The vertex to delete.</param>
        private void DeleteVertex(int vertex)
        {
            // if not last vertex, delete from the vertices list  
            if (vertex != _numVerts - 1)
            {
                for (var j = vertex; j < _numVerts - 1; j++)
                {
                    _vertices[j] = _vertices[j + 1];
                }

                for (var row = vertex; row < _numVerts - 1; row++)
                {
                    MoveRowUp(row, _numVerts);
                }

                for (var col = vertex; col < _numVerts - 1; col++)
                {
                    MoveColLeft(col, _numVerts - 1);
                }
            }

            // one less vertex
            _numVerts--;
        }

        /// <summary>
        /// Moves a row up in the adjacency matrix.
        /// </summary>
        /// <param name="row">The row to move.</param>
        /// <param name="length">The column count.</param>
        private void MoveRowUp(int row, int length)
        {
            for (var col = 0; col < length; col++)
            {
                _matrix[row, col] = _matrix[row + 1, col];
            }
        }

        /// <summary>
        /// Moves a column to the left in the adjacency matrix.
        /// </summary>
        /// <param name="col">The column to move.</param>
        /// <param name="length">The row count.</param>
        private void MoveColLeft(int col, int length)
        {
            for (var row = 0; row < length; row++)
            {
                _matrix[row, col] = _matrix[row, col + 1];
            }
        }

        #endregion
    }
}
