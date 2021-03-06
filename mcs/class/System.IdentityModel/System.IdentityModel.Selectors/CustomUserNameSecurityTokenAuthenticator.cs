//
// CustomUserNameSecurityTokenAuthenticator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public class CustomUserNameSecurityTokenAuthenticator
		: UserNameSecurityTokenAuthenticator
	{
		UserNamePasswordValidator validator;

		public CustomUserNameSecurityTokenAuthenticator (
			UserNamePasswordValidator validator)
		{
			if (validator == null)
				throw new ArgumentNullException ("validator");
			this.validator = validator;
		}

		protected override ReadOnlyCollection<IAuthorizationPolicy>
			ValidateUserNamePasswordCore (string userName, string password)
		{
			validator.Validate (userName, password);
			IAuthorizationPolicy policy =
				new AuthorizedCustomUserPolicy (userName);
			return new ReadOnlyCollection<IAuthorizationPolicy> (new IAuthorizationPolicy [] {policy});
		}

		abstract class SystemIdentityAuthorizationPolicy : IAuthorizationPolicy
		{
			string id;

			protected SystemIdentityAuthorizationPolicy (string id)
			{
				this.id = id;
			}

			public string Id {
				get { return id; }
			}

			public ClaimSet Issuer {
				get { return ClaimSet.System; }
			}


			// This method is expected to be thread safe
			public bool Evaluate (EvaluationContext ec, ref object state)
			{
				lock (ec) {
					ec.AddClaimSet (this, CreateClaims ());
					List<IIdentity> list;
					if (!ec.Properties.ContainsKey ("Identities")) {
						list = new List<IIdentity> ();
						ec.Properties ["Identities"] = list;
					} else {
						IList<IIdentity> ilist = (IList<IIdentity>) ec.Properties ["Identities"];
						list = ilist as List<IIdentity>;
						if (list == null) {
							list = new List<IIdentity> (ilist);
							ec.Properties ["Identities"] = list;
						}
					}
					list.Add (CreateIdentity ());
					ec.RecordExpirationTime (DateTime.MaxValue.AddDays (-1));
				}
				// FIXME: is it correct that this should always return true?
				return true;
			}

			public abstract DateTime ExpirationTime { get; }

			public abstract ClaimSet CreateClaims ();

			public abstract IIdentity CreateIdentity ();
		}

		class AuthorizedCustomUserPolicy : SystemIdentityAuthorizationPolicy
		{
			string user;

			public AuthorizedCustomUserPolicy (string user)
				: base (new UniqueId ().ToString ())
			{
				this.user = user;
			}

			public override DateTime ExpirationTime {
				get { return DateTime.MaxValue; }
			}

			public override ClaimSet CreateClaims ()
			{
				return new DefaultClaimSet (Claim.CreateNameClaim (user));
			}

			public override IIdentity CreateIdentity ()
			{
				return new GenericIdentity (user);
			}
		}
	}
}
