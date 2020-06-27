using Microsoft.AspNetCore.Identity;
using MyCNBlog.Core.Interfaces;
using System;
using System.Collections;

namespace MyCNBlog.Core.Models
{
    public class BlogUser : IdentityUser<int>, IEntity<int>
    {
        public bool IsDeleted { get; set; }

        public Sex Sex { get; set; }

        public DateTime Birth { get; set; }

        public DateTime RegiserDate { get; set; }

        /// <summary>
        /// Physcial path of avatar
        /// </summary>
        public string AvatarPath { get; set; }

        /// <summary>
        /// One user has one Blog, One Blog belong to one user
        /// </summary>
        public virtual Blog Blog { get; set; }


        public BlogUser(string name, DateTime birth, Sex sex)
        {
            Birth = birth;
            UserName = name;
            Sex = sex;
            RegiserDate = DateTime.Now;
            NormalizedUserName = UserName?.ToUpper();
            Blog = new Blog();
        }

        public BlogUser(string name) : this(name, default)
        {
        }

        public BlogUser(string name, DateTime birth):this(name, birth, Sex.Unknown)
        {
        }

        public BlogUser():this(default, default)
        {
        }
    }
}
