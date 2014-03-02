﻿using System;
using EnvDTE;
using EnvDTE80;

namespace T4ToRgen.Extension
{
    internal static class ProjectSolutionExtensions
    {

        /// <summary>
        /// Get solution name
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static string GetName(this Solution solution)
        {
            return solution.Properties.Item("Name").Value.ToString();
        }
        /// <summary>
        /// Get solution
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static Solution Solution(this Project project)
        {
            return project.DTE.Solution;
        }
        /// <summary>
        /// Get path to project node. To be used to select the node in Solution Explorer
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static string GetNodePath(this Project project)
        {
            return string.Format("{0}\\{1}", project.Solution().GetName(), project.Name);
        }
        /// <summary>
        /// Get path to project item node. To be used to select the node in Solution Explorer
        /// </summary>
        /// <param name="projectItem"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static string GetNodePath(this ProjectItem projectItem)
        {
            return string.Format("{0}\\{1}", projectItem.ContainingProject.GetNodePath(), projectItem.Name);
        }
        /// <summary>
        /// Selects project item in Solution Explorer
        /// </summary>
        /// <param name="projectItem"></param>
        /// <remarks></remarks>
        
        public static void SelectSolutionExplorerNode(this ProjectItem projectItem)
        {
            ((DTE2)projectItem.DTE).SelectSolutionExplorerNode(projectItem.GetNodePath());
        }
        /// <summary>
        /// Selects project item in Solution Explorer
        /// </summary>
        /// <remarks></remarks>
        
        public static void SelectSolutionExplorerNode(this DTE2 dte2, string nodePath)
        {
            try
            {
                var item = dte2.ToolWindows.SolutionExplorer.GetItem(nodePath);
                item.Select(vsUISelectionType.vsUISelectionTypeSelect);
            }
            catch (Exception)
            {
                DebugExtensions.DebugHere();
            }
        }
    }
}
