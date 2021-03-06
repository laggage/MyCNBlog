﻿using MyCNBlog.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace MyCNBlog.Core.Models
{
    public class Blog : Entity
    {
        public DateTime OpenDate { get; set; }

        private bool _isOpen;
        /// <summary>
        /// 博客功能是否开通, default: false
        /// </summary>
        public bool IsOpened
        {
            get => _isOpen; set
            {
                if(value)
                    OpenDate = DateTime.Now;
                _isOpen = value;
            }
        }

        /// <summary>
        /// 博客签名
        /// </summary>
        public string Sign { get; set; }

        public int UserId { get; set; }


        public virtual ICollection<Post> Posts { get; set; }

        public virtual BlogUser User { get; set; }
    }
}
